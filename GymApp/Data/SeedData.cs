using GymApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace GymApp.Data
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = services.GetRequiredService<UserManager<Member>>();
            var db = services.GetRequiredService<GymDbContext>();

            if (!await roleMgr.RoleExistsAsync("Admin"))
                await roleMgr.CreateAsync(new IdentityRole("Admin"));
            if (!await roleMgr.RoleExistsAsync("Member"))
                await roleMgr.CreateAsync(new IdentityRole("Member"));

            var email = "b231210016@sakarya.edu.tr";
            var admin = await userMgr.FindByEmailAsync(email);
            if (admin == null)
            {
                admin = new Member
                {
                    UserName = email,
                    Email = email,
                    FullName = "Admin Kullanıcı",
                    CreatedAt = DateTime.UtcNow
                };

                await userMgr.CreateAsync(admin, "sau");
                await userMgr.AddToRoleAsync(admin, "Admin");
            }

            // --- Seed Services ---
            if (!db.Services.Any())
            {
                var servicesToAdd = new List<Service>
                {
                    new Service
                    {
                        Name = "Kişisel Antrenman",
                        Description = "Birebir koçluk, hedef odaklı antrenman programı.",
                        ServiceType = "Personal Training",
                        DurationMinutes =60,
                        Price =200m,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "Grup Yoga",
                        Description = "Esneklik ve nefes çalışmaları.",
                        ServiceType = "Yoga",
                        DurationMinutes =45,
                        Price =75m,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "HIIT Seansı",
                        Description = "Yüksek yoğunluklu interval antrenman.",
                        ServiceType = "Fitness",
                        DurationMinutes =30,
                        Price =90m,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "Pilates Mat",
                        Description = "Core güçlendirme ve postür düzeltme.",
                        ServiceType = "Pilates",
                        DurationMinutes =50,
                        Price =120m,
                        IsActive = true
                    }
                };

                db.Services.AddRange(servicesToAdd);
                await db.SaveChangesAsync();
            }

            // --- Seed Gyms ---
            if (!db.Gyms.Any())
            {
                var gymsToAdd = new List<Gym>
                {
                    new Gym
                    {
                        Name = "Central Fit Club",
                        Address = "Merkez Mah. Spor Cad. No:12, Sakarya",
                        WorkingHours = "Hafta içi06:00 -23:00, Hafta sonu08:00 -20:00",
                        Phone = "+902640000000",
                        Email = "info@centralfit.com",
                        Description = "Şehrin en donanımlı salonu, deneyimli eğitmenler ve geniş ekipman alanı.",
                        PhotoUrl = null
                    },
                    new Gym
                    {
                        Name = "Green Wellness",
                        Address = "Park Yolu5, Sakarya",
                        WorkingHours = "07:00 -22:00",
                        Phone = "+902641111111",
                        Email = "hello@greenwellness.com",
                        Description = "Yoga ve pilates odaklı huzurlu bir ortam.",
                        PhotoUrl = null
                    }
                };

                db.Gyms.AddRange(gymsToAdd);
                await db.SaveChangesAsync();
            }

            // --- Seed Trainers ---
            if (!db.Trainers.Any())
            {
                // pick gyms
                var gym1 = db.Gyms.FirstOrDefault();
                var gym2 = db.Gyms.Skip(1).FirstOrDefault() ?? gym1;

                var trainersToAdd = new List<Trainer>
                {
                    new Trainer
                    {
                        FullName = "Ahmet Yılmaz",
                        Expertise = "Kuvvet & Kondisyon",
                        ExperienceYears =8,
                        Phone = "+905320000000",
                        Email = "ahmet.yilmaz@example.com",
                        Bio = "Spor bilimi eğitimi almış, sporcularla çalışma deneyimi olan antrenör.",
                        GymId = gym1.Id
                    },
                    new Trainer
                    {
                        FullName = "Elif Demir",
                        Expertise = "Yoga & Pilates",
                        ExperienceYears =5,
                        Phone = "+905321111111",
                        Email = "elif.demir@example.com",
                        Bio = "Ruh ve beden dengesini önemseyen yoga eğitmeni.",
                        GymId = gym2.Id
                    },
                    new Trainer
                    {
                        FullName = "Mehmet Kara",
                        Expertise = "HIIT & Fonksiyonel",
                        ExperienceYears =4,
                        Phone = "+905322222222",
                        Email = "mehmet.kara@example.com",
                        Bio = "Kısa ve etkili antrenman programları uzmanı.",
                        GymId = gym1.Id
                    }
                };

                db.Trainers.AddRange(trainersToAdd);
                await db.SaveChangesAsync();
            }

            // --- Seed GymServices (which services are offered at gyms) ---
            if (!db.GymServices.Any())
            {
                var gyms = db.Gyms.Take(2).ToList();
                var servicesAll = db.Services.Take(4).ToList();

                var gymServicesToAdd = new List<GymService>();

                if (gyms.Count >0 && servicesAll.Count >0)
                {
                    // Add first two services to first gym
                    gymServicesToAdd.Add(new GymService
                    {
                        GymId = gyms[0].Id,
                        ServiceId = servicesAll[0].Id,
                        CustomPrice = null,
                        IsActive = true
                    });
                    gymServicesToAdd.Add(new GymService
                    {
                        GymId = gyms[0].Id,
                        ServiceId = servicesAll[2].Id,
                        CustomPrice =85m,
                        IsActive = true
                    });

                    if (gyms.Count >1)
                    {
                        // Add yoga & pilates to second gym
                        var yoga = servicesAll.FirstOrDefault(s => s.ServiceType == "Yoga") ?? servicesAll.ElementAtOrDefault(1);
                        var pilates = servicesAll.FirstOrDefault(s => s.ServiceType == "Pilates") ?? servicesAll.ElementAtOrDefault(3);

                        if (yoga != null)
                            gymServicesToAdd.Add(new GymService { GymId = gyms[1].Id, ServiceId = yoga.Id, IsActive = true });
                        if (pilates != null)
                            gymServicesToAdd.Add(new GymService { GymId = gyms[1].Id, ServiceId = pilates.Id, IsActive = true });
                    }
                }

                if (gymServicesToAdd.Any())
                {
                    db.GymServices.AddRange(gymServicesToAdd);
                    await db.SaveChangesAsync();
                }
            }

            // --- Seed TrainerServices (which services each trainer can provide) ---
            if (!db.TrainerServices.Any())
            {
                var trainers = db.Trainers.Take(5).ToList();
                var servicesAll = db.Services.ToList();
                var trainerServicesToAdd = new List<TrainerService>();

                if (trainers.Any() && servicesAll.Any())
                {
                    // Map first trainer to personal training and HIIT
                    var t1 = trainers.ElementAtOrDefault(0);
                    if (t1 != null)
                    {
                        var personal = servicesAll.FirstOrDefault(s => s.ServiceType.Contains("Personal")) ?? servicesAll.FirstOrDefault();
                        var hiit = servicesAll.FirstOrDefault(s => s.Name.Contains("HIIT"));
                        if (personal != null) trainerServicesToAdd.Add(new TrainerService { TrainerId = t1.Id, ServiceId = personal.Id });
                        if (hiit != null) trainerServicesToAdd.Add(new TrainerService { TrainerId = t1.Id, ServiceId = hiit.Id });
                    }

                    // Map second trainer to yoga/pilates
                    var t2 = trainers.ElementAtOrDefault(1);
                    if (t2 != null)
                    {
                        var yoga = servicesAll.FirstOrDefault(s => s.ServiceType == "Yoga") ?? servicesAll.ElementAtOrDefault(1);
                        var pilates = servicesAll.FirstOrDefault(s => s.ServiceType == "Pilates") ?? servicesAll.ElementAtOrDefault(3);
                        if (yoga != null) trainerServicesToAdd.Add(new TrainerService { TrainerId = t2.Id, ServiceId = yoga.Id });
                        if (pilates != null) trainerServicesToAdd.Add(new TrainerService { TrainerId = t2.Id, ServiceId = pilates.Id });
                    }

                    // Map third trainer to HIIT
                    var t3 = trainers.ElementAtOrDefault(2);
                    if (t3 != null)
                    {
                        var hiit = servicesAll.FirstOrDefault(s => s.Name.Contains("HIIT")) ?? servicesAll.ElementAtOrDefault(2);
                        if (hiit != null) trainerServicesToAdd.Add(new TrainerService { TrainerId = t3.Id, ServiceId = hiit.Id });
                    }
                }

                if (trainerServicesToAdd.Any())
                {
                    db.TrainerServices.AddRange(trainerServicesToAdd);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
